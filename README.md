# pubsubbing
test app to practice using pub/sub and event messaging processes


## Commands

```
docker run -p 15672:15672 -p 5672:5672 masstransit/rabbitmq
```

ARM Platform
```
docker run --platform linux/arm64 -p 15672:15672 -p 5672:5672 masstransit/rabbitmq
```