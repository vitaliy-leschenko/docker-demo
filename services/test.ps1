$cid = (docker ps --filter "status=running" --filter "name=demo-api" -q)
$ip = (docker inspect --format="{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}" $cid)

for ($i = 0; $i -lt 10; $i += 1) {
   $obj = @{ comment = (Get-Date).ToString() }
   $json = ConvertTo-Json $obj

   Invoke-RestMethod -Method Post -Uri "http://$ip/api/tasks" -Body $json -ContentType application/json
}
