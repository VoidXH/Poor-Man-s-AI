<?php
if (!$admin) {
    die;
}

if (isset($_GET["del"])) {
    execute("DELETE FROM ai_reports WHERE time = ?", $_GET["del"]);
}
?>
<h2>Disliked chat messages</h1>
<script src="<?=$jqueryPath ?>"></script>
<script>
function show(id) {
    $.get("admin/dislike_dump.php?id=" + id, function(response) {
        let form = $('<form action="chat.php" method="POST"></form>');
        let input = $('<input type="hidden" name="fill">').val(response);
        form.append(input);
        $("body").append(form);
        form.submit();
    });
}
</script>
<table class="table table-striped table-dark">
    <thead>
        <tr>
            <th>Time</th>
            <th>Conversation</th>
            <th>Action</th>
        </tr>
    </thead>
    <tbody>
<?php
$result = $sqlink->query("SELECT time, LEFT(data, 100) AS data FROM ai_reports ORDER BY time DESC");
if ($result->num_rows > 0) {
    while($row = $result->fetch_assoc()) {
        $time = $row["time"];
        $conversation = htmlspecialchars($row["data"]);
        echo "<tr><td>$time</td><td>$conversation</td>";
        echo "<td><button class=\"btn btn-primary\" onclick=\"show('$time')\">Show</button>";
        echo "<a class=\"btn btn-danger\" href=\"?p=3&del=$time\">Delete</a></td></tr>";
    }
} else {
    echo "<tr><td colspan=\"3\">No reports found.</td></tr>";
}
?>
    </tbody>
</table>