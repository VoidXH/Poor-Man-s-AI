<?php
if (!$admin) {
    die;
}
if (isset($_GET["wipeload"])) {
    $sqlink->query("DELETE FROM ai_usage");
}
?>
<h2>Usage Tracker</h2>
<p class="text-secondary">Graphs the server load (maximum number of commands in queue) over time. This helps to check if your Processor node is able to keep up with demand or needs an update. Elevated usage can also be an indicator of DDoS attacks.</p>
<?php if (!$usageTracking) { ?>
<div class="alert alert-warning mt-3">
    Usage tracking is <strong>disabled</strong>. Enable it in the Config tab to start collecting statistics and show the graph.
</div>
<?php } else {
    $date = isset($_GET["load"]) ? $_GET["load"] : date("Y-m-d");
    $stmt = $sqlink->prepare("SELECT * FROM ai_usage WHERE `time` BETWEEN ? AND ?");
    $start = "$date 00:00:00";
    $end = "$date 23:59:59";
    $stmt->bind_param("ss", $start, $end);
    $stmt->execute();
    $result = $stmt->get_result();
    $data = [];
    while ($row = $result->fetch_assoc()) {
        $time = substr($row["time"], 11, 5);
        $usage = (int)$row["usage"];
        $data[] = ["time" => $time, "usage" => $usage];
    }
    $stmt->close();
    $jsonData = json_encode($data);
?>
<div class="card h-auto mb-3">
    <div class="card-body">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <input id="date" type="date" onchange="onDate()" value="<?=$date ?>" class="form-control form-control-sm w-auto">
            <a class="btn btn-outline-danger btn-sm" href="?p=5&wipeload">Delete History</a>
        </div>
        <div style="position: relative; height: 200px; overflow: hidden;">
            <canvas id="usageChart"></canvas>
        </div>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
<script>
function onDate() {
  window.location.href = "?p=5&load=" + $("#date").val();
}

var usageData = <?php echo $jsonData; ?>;
var times = [];
var usages = [];
usageData.forEach(function(entry) {
    times.push(entry.time);
    usages.push(entry.usage);
});

var ctx = document.getElementById('usageChart').getContext('2d');
var usageChart = new Chart(ctx, {
    type: 'line',
    data: {
        labels: times,
        datasets: [{
            label: 'Usage',
            data: usages,
            borderColor: 'rgba(0, 200, 150, 1)',
            backgroundColor: 'rgba(0, 200, 150, 0.15)',
            fill: true,
            tension: 0.2
        }]
    },
    options: {
        maintainAspectRatio: false,
        responsive: true,
        scales: {
            x: {
                title: {
                    display: true,
                    text: 'Time of Day',
                    color: '#7e8da0'
                },
                ticks: { color: '#7e8da0' },
                grid: { color: 'rgba(255,255,255,0.05)' }
            },
            y: {
                beginAtZero: true,
                title: {
                    display: true,
                    text: 'Max Queue Length',
                    color: '#7e8da0'
                },
                ticks: { color: '#7e8da0', stepSize: 1 },
                grid: { color: 'rgba(255,255,255,0.05)' }
            }
        },
        plugins: {
            legend: { labels: { color: '#e4eaf0' } }
        }
    }
});
</script>
<?php
}
?>
