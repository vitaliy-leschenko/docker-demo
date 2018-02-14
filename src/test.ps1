$cid = (docker ps --filter "status=running" --filter "name=demo.web" -q)
$ip = (docker inspect --format="{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}" $cid)

$obj = @{ comment = (Get-Date).ToString() }
$json = ConvertTo-Json $obj

$model = Invoke-RestMethod -Method Post -Uri "http://$ip/api/tasks" -Body $json -ContentType application/json
while (($model.status -eq 0) -or ($model.status -eq 1)) {
    Write-Host "Status: $($model.status), Progress: $($model.progress)"

    $model = Invoke-RestMethod -Method Get -Uri "http://$ip/api/tasks/$($model.id)" -ContentType application/json
}
Write-Host "Status: $($model.status), Progress: $($model.progress)"