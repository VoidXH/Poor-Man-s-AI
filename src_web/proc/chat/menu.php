<div class="dropdown">
    <button class="btn btn-sm btn-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
        <span class="fas fa-ellipsis-vertical"></span>
    </button>
    <ul class="dropdown-menu dropdown-menu-end">
<?php if ($uid) { ?>
        <li><a class="dropdown-item" href="profile.php"><?=htmlspecialchars($_COOKIE["username"]) ?></a></li>
<?php if ($admin) { ?>
        <li><a class="dropdown-item" href="admin.php">Admin panel</a></li>
<?php } ?>
        <li><a class="dropdown-item" href="logout.php">Logout</a></li>
<?php } else { ?>
        <li><a class="dropdown-item" href="login.php">Login</a></li>
<?php } ?>
        <li><hr class="dropdown-divider"></li>
        <li><a class="dropdown-item" onclick="reset()">New chat</a></li>
        <li><hr class="dropdown-divider"></li>
<?php if ($admin) { ?>
        <li><a class="dropdown-item" href="agent.php">Remote agent</a></li>
<?php } ?>
        <li><a class="dropdown-item" href="image.php">Custom images</a></li>
        <li><hr class="dropdown-divider"></li>
        <li>
            <div class="theme-swatches">
                <button type="button" class="theme-swatch theme-swatch-green" data-theme="green" title="Green theme"></button>
                <button type="button" class="theme-swatch theme-swatch-blue" data-theme="blue" title="Blue theme"></button>
                <button type="button" class="theme-swatch theme-swatch-red" data-theme="red" title="Red theme"></button>
            </div>
        </li>
    </ul>
</div>
<script src="js/menu.js"></script>