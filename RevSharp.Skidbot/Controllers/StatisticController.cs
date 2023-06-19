﻿using Prometheus;
using RevSharp.Skidbot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevSharp.Skidbot.Controllers
{
    [RevSharpModule]
    public class StatisticController : BaseModule
    {
        public override async Task Initialize(ReflectionInclude reflection)
        {
            InternalInit();
        }
        private async Task InternalInit()
        {
            bool isNull = true;
            while (isNull)
            {
                isNull = Client == null || Client.CurrentUser == null;
                await Task.Delay(500);
            }
            
            MetricServer = new KestrelMetricServer(port: Program.ConfigData.PrometheusPort);
            if (Program.ConfigData.PrometheusEnable)
            {
                MetricServer.Start();
            }
            await InitMetrics();

            await UpdateData();
            Observable.Interval(TimeSpan.FromSeconds(60))
                .Subscribe(_ => UpdateData().Wait());
        }
        private async Task InitMetrics()
        {
            ServerMemberGauge = CreateGauge(
                "skid_revolt_server_members",
                    "Amount of members per server",
                    labelNames: new string[]
                    {
                        "server_name",
                        "server_id"
                    },
                    publish: false);
            ServerGauge = CreateGauge(
                "skid_revolt_server_count",
                "Amount of servers this bot is in",
                publish: false);
            CommandCounter = CreateCounter(
                "skid_revolt_command_count",
                "Commands received",
                labelNames: new string[]
                {
                    "server_name",
                    "server_id",
                    "author_name",
                    "author_id",
                    "channel_name",
                    "channel_id",
                    "command_name",
                    "command_args",
                    "command_category"
                }, publish: false);
        }

        private KestrelMetricServer MetricServer;

        public int ServerCount = 0;
        /// <summary>
        /// Key: Server Id
        /// Value: Member Count
        /// </summary>
        public Dictionary<string, int> MemberCount = new Dictionary<string, int>();

        public Gauge ServerMemberGauge;
        public Gauge ServerGauge;
        public Counter CommandCounter;

        private async Task UpdateData()
        {
            var servers = await Client.GetAllServers();
            var memberDict = new Dictionary<string, int>();
            foreach (var s in servers)
            {
                var members = await s.FetchMembers(false);
                if (members != null)
                {
                    memberDict.Add(s.Id, members.Count);
                    ServerMemberGauge.WithLabels(new string[]
                    {
                        s.Name,
                        s.Id
                    }).Set(members.Count);
                }
            }
            ServerGauge.Set(servers.Count);
            ServerCount = memberDict.Count;
            MemberCount = memberDict;
        }

        #region MetricServer Wrapper Methods
        public Counter CreateCounter(string name, string help, string[]? labelNames = null, CounterConfiguration? config = null, bool publish = false)
        {
            var c = Metrics.CreateCounter(
                name,
                help,
                labelNames ?? Array.Empty<string>(),
                config);
            if (publish)
                c.Publish();
            return c;
        }
        public Gauge CreateGauge(string name, string help, string[]? labelNames = null, GaugeConfiguration? config = null, bool publish = false)
        {
            var g = Metrics.CreateGauge(
                name,
                help,
                labelNames ?? Array.Empty<string>(),
                config);
            if (publish)
                g.Publish();
            return g;
        }
        public Summary CreateSummary(string name, string help, string[]? labelNames = null, SummaryConfiguration? config = null, bool publish = false)
        {
            var s = Metrics.CreateSummary(
                name,
                help,
                labelNames ?? Array.Empty<string>(),
                config);
            if (publish)
                s.Publish();
            return s;
        }
        public Histogram CreateHistogram(string name, string help, string[]? labelNames = null, HistogramConfiguration? config = null, bool publish = false)
        {
            var h = Metrics.CreateHistogram(
                name,
                help,
                labelNames ?? Array.Empty<string>(),
                config);
            if (publish)
                h.Publish();
            return h;
        }
        #endregion
    }
}