using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics.Metrics;

namespace ExtravaWall;
public class ExtravaMetrics {

    private readonly Counter<int> _productSoldCounter;

    public ExtravaMetrics(IMeterFactory meterFactory) {
        var meter = meterFactory.Create("ExtravaWallRouter");
        _productSoldCounter = meter.CreateCounter<int>("extrava_packets_inspected");
    }

    public void PacketInspected(string packetTcpProtocol, int quantity) {
        _productSoldCounter.Add(quantity,
            new KeyValuePair<string, object?>("extrava_packet_tcp_protocol", packetTcpProtocol));
    }

}
