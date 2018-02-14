$cid = (docker ps --filter "status=running" --filter "name=demo-api" -q)
$ip = (docker inspect --format="{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}" $cid)

$obj = @{ comment = (Get-Date).ToString() }
$json = ConvertTo-Json $obj

$model = Invoke-RestMethod -Method Post -Uri "http://$ip/api/tasks" -Body $json -ContentType application/json
while (($model.status -eq 0) -or ($model.status -eq 1)) {
    Write-Progress -Activity "Demo task processing..." -PercentComplete $($model.progress)

    $model = Invoke-RestMethod -Method Get -Uri "http://$ip/api/tasks/$($model.id)" -ContentType application/json
    Start-Sleep -Milliseconds 250
}
if ($model.status -eq 2) {
    Write-Host "Task failed" -ForegroundColor Red
} else {
    Write-Progress -Activity "Demo task processing..." -PercentComplete 100
    Write-Host "Task completed" -ForegroundColor Green
}

