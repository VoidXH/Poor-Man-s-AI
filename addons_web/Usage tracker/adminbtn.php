<a class="btn btn-secondary" href="?load">Load</a>
<?php
if (isset($_GET["load"])) {
	$date = $_GET["load"];
	$date = $date == "" ? date("Y-m-d") : $date;
	$result = $sqlink->query("SELECT * FROM ai_usage WHERE `time` BETWEEN '$date 00:00:00' AND '$date 23:59:59'");
	$data = [];
	while ($row = mysqli_fetch_assoc($result)) {
		$time = substr($row["time"], 11, 5);
		$usage = (int)$row["usage"];
		$data[] = ["time" => $time, "usage" => $usage];
	}
	$jsonData = json_encode($data);
?>
<div class="p-2 text-center w-50" style="position: absolute; top: 50vh; left: 50vw; transform: translate(-50%, -50%); border-radius: 25px; background-color: #ffffffbf;">
	<input id="date" type="date" onchange="onDate()" value="<?=$date ?>">
	<canvas id="usageChart"></canvas>
	<a class="btn btn-secondary" href="?wipeload">Delete history</a>
</div>
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
<script>
function onDate() {
  window.location.href = "?load=" + $("#date").val();
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
<?php }
if (isset($_GET["wipeload"])) {
	$sqlink->query("DELETE FROM ai_usage");
}
?>