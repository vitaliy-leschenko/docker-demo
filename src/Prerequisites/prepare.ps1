@("rabbit-mq", "mssql-server") | %{
    $cid = (docker ps --filter "name=$_" -q)
    if ($cid -ne $null) {
        Write-Host "Remove $_ container" -ForegroundColor Yellow
        & docker rm $cid --force
    }
}

Write-Host "Building new images..." -ForegroundColor Green

docker build -f .\Prerequisites\MSSQL\Dockerfile -t mssql-server .
docker build -f .\Prerequisites\RabbitMQ\Dockerfile -t rabbitmq .

Write-Host "Creating MSSQL..." -ForegroundColor Green

$cid = (docker run -d `
           --name mssql-server `
           --hostname mssql `
           --volume "$PWD/volumes/mssql:c:/data" `
           --env "ACCEPT_EULA=Y" `
           --env "sa_password=nRhtG1c9" `
           --env "attach_dbs=[{'dbName':'demodb','dbFiles':['C:\\data\\demodb.mdf', 'C:\\data\\demodb_log.ldf']}]" `
           --restart on-failure `
           mssql-server)

$ip = (docker inspect --format="{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}" $cid)
Write-Host "MSSQL Server: $ip"

Write-Host "Creating RabbitMQ..." -ForegroundColor Green

$cid = (docker run -d `
           --name rabbit-mq `
           --hostname rabbitmq `
           --volume "$PWD/volumes/rabbitmq-db:c:/RabbitMQ-data/db" `
           --volume "$PWD/volumes/rabbitmq-log:c:/RabbitMQ-data/log" `
           --restart on-failure `
           rabbitmq)

$ip = (docker inspect --format="{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}" $cid)
Write-Host "RabbitMQ Server: http://$($ip):15672"

