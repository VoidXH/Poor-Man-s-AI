<?php
if (!$admin) {
    die;
}
?>
<h2>Usage tracker</h2>
<p>Graphs the server load (maximum number of commands in queue) over time. This
helps to check if your Processor node is able to keep up with demand or needs an
update. Elevated usage can also be an indicator of DDoS attacks.</p>
<?php if (!$usageTracking) { ?>
<div class="alert alert-warning mt-3">
    Usage tracking is <strong>disabled</strong>. Enable it in the Config tab to
    start collecting statistics and show the graph.
</div>
<?php } else {
	$date = isset($_GET["load"]) ? $_GET["load"] : date("Y-m-d");
	$result = $sqlink->query("SELECT * FROM ai_usage WHERE `time` BETWEEN '$date 00:00:00' AND '$date 23:59:59'");
	$data = [];
	while ($row = mysqli_fetch_assoc($result)) {
		$time = substr($row["time"], 11, 5);
		$usage = (int)$row["usage"];
		$data[] = ["time" => $time, "usage" => $usage];
	}
	$jsonData = json_encode($data);
?>
<div class="p-2 text-center w-50" style="border-radius: 25px; background-color: #ffffffbf;">
	<input id="date" type="date" onchange="onDate()" value="<?=$date ?>">
	<canvas id="usageChart"></canvas>
	<a class="btn btn-secondary" href="?p=5&wipeload">Delete history</a>
</div>
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
<script src="<?=$jqueryPath ?>"></script>
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
			borderColor: 'rgba(75, 192, 192, 1)',
			backgroundColor: 'rgba(75, 192, 192, 0.2)',
			fill: true,
			tension: 0.1
		}]
	},
	options: {
		scales: {
			x: {
				title: {
					display: true,
					text: 'Time of Day'
				}
			},
			y: {
				beginAtZero: true,
				title: {
					display: true,
					text: 'Max Queue Length'
				},
				ticks: {
					stepSize: 1
				}
			}
		}
	}
});
</script>
<?php
}
if (isset($_GET["wipeload"])) {
	$sqlink->query("DELETE FROM ai_usage");
}
?>
