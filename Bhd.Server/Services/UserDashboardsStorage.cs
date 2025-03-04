﻿using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Bhd.Server.Hubs;
using Bhd.Shared;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace Bhd.Server.Services {
    public class UserDashboardsStorage {
        public List<DashboardConfig> Dashboards { get; private set; }

        private readonly IHubContext<NotificationsHub> _notificationsHub;

        public UserDashboardsStorage(IHubContext<NotificationsHub> notificationsHub) {
            _notificationsHub = notificationsHub;

            if (File.Exists("UserDashboards.json")) {
                Dashboards = JsonSerializer.Deserialize<List<DashboardConfig>>(File.ReadAllText("UserDashboards.json"));
            } else {
                Dashboards = new List<DashboardConfig>();
            }
        }

        public void UpdateDashboards(List<DashboardConfig> userDashboards) {
            File.WriteAllText("UserDashboards.json", JsonSerializer.Serialize(userDashboards, new JsonSerializerOptions { WriteIndented = true }));
            Dashboards = userDashboards;
            _notificationsHub.Clients.All.SendAsync("DashboardConfigurationChanged");
        }
    }
}