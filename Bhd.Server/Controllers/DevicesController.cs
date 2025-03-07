﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Bhd.Server.Services;
using Bhd.Shared;
using Bhd.Shared.DTOs;
using DevBot9.Protocols.Homie;
using Device = Bhd.Shared.DTOs.Device;

namespace Bhd.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase {
        private readonly ILogger<DevicesController> _logger;
        private readonly HomieService _homieService;

        public DevicesController(ILogger<DevicesController> logger, HomieService homieService) {
            _logger = logger;
            _homieService = homieService;
        }

        [HttpGet]
        public IEnumerable<Device> Get() {
            var devices = new List<Device>();

            foreach (var homieServiceDynamicConsumer in _homieService.DynamicConsumers) {
                var device = new Device();
                device.Id = homieServiceDynamicConsumer.ClientDevice.DeviceId;
                device.Name = homieServiceDynamicConsumer.ClientDevice.Name;
                device.Nodes = $"/api/devices/{device.Id}/nodes";

                switch (homieServiceDynamicConsumer.ClientDevice.State) {
                    case HomieState.Ready:
                        device.State = DeviceState.Ready;
                        devices.Add(device);
                        break;

                    case HomieState.Alert:
                        device.State = DeviceState.Alert;
                        devices.Add(device);
                        break;

                    case HomieState.Lost:
                        device.State = DeviceState.Lost;
                        devices.Add(device);
                        break;

                    default:
                        // Unsupported state, skipping.
                        break;
                }
            }


            return devices;
        }

        [HttpPost("Rescan")]
        public void Rescan() {
            _homieService.Rescan();
        }

        [HttpGet("{deviceId}")]
        public Device GetDevice(string deviceId) {
            return Get().First(d => d.Id == deviceId);
        }

        [HttpGet("{deviceId}/Nodes")]
        public IEnumerable<Node> GetNodes(string deviceId) {
            var dynamicConsumer = _homieService.DynamicConsumers.First(d => d.ClientDevice.DeviceId == deviceId);
            var nodes = new List<Node>();
            foreach (var clientDeviceNode in dynamicConsumer.ClientDevice.Nodes) {
                var node = new Node();
                node.Name = clientDeviceNode.Name;
                node.Id = clientDeviceNode.NodeId;

                foreach (var clientPropertyBase in clientDeviceNode.Properties) {
                    node.Properties.Add($"/api/devices/{deviceId}/nodes/{node.Id}/properties/{clientPropertyBase.PropertyId.Replace($"{node.Id}/", "")}");
                }

                nodes.Add(node);
            }

            return nodes;
        }

        [HttpGet("{deviceId}/Nodes/{nodeId}")]
        public Node GetNode(string deviceId, string nodeId) {
            var nodes = GetNodes(deviceId);
            return nodes.First(n => n.Id == nodeId);
        }

        [HttpGet("{deviceId}/Nodes/{nodeId}/Properties")]
        public IEnumerable<string> GetProperties(string deviceId, string nodeId) {
            var node = GetNode(deviceId, nodeId);
            return node.Properties;
        }

        [HttpGet("{deviceId}/Nodes/{nodeId}/Properties/{propertyId}")]
        public Property GetProperty(string deviceId, string nodeId, string propertyId) {
            var propertyBase = GetPropertyBase(deviceId, nodeId, propertyId);
            var property = PropertyFactory.Create(propertyBase);
            return property;
        }

        [HttpPut("{deviceId}/Nodes/{nodeId}/Properties/{propertyId}/TextValue")]
        public void SetTextValue(string deviceId, string nodeId, string propertyId, [FromBody]string textValue) {
            var property = GetPropertyBase(deviceId, nodeId, propertyId);

            switch (property) {
                case ClientChoiceProperty choiceProperty:
                    choiceProperty.Value = textValue;
                    break;

                case ClientTextProperty textProperty:
                    textProperty.Value = textValue;
                    break;
            }
        }

        [HttpPut("{deviceId}/Nodes/{nodeId}/Properties/{propertyId}/NumericValue")]
        public void SetNumericValue(string deviceId, string nodeId, string propertyId, [FromBody] double numericValue) {
            var property = GetPropertyBase(deviceId, nodeId, propertyId);

            switch (property) {
                case ClientNumberProperty numberProperty:
                    numberProperty.Value = numericValue;
                    break;
            }
        }

        private ClientPropertyBase GetPropertyBase(string deviceId, string nodeId, string propertyId) {
            propertyId = $"{nodeId}/{propertyId}";
            var device = _homieService.DynamicConsumers.First(d => d.ClientDevice.DeviceId == deviceId);
            var node = device.ClientDevice.Nodes.First(n => n.NodeId == nodeId);
            var property = node.Properties.First(p => p.PropertyId == propertyId);
            return property;
        }
    }
}