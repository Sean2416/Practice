# 使用 base image 為 alpine 3.9 且 redis 為 5.0.3 的版本
FROM redis:5.0.3-alpine3.9
MAINTAINER Yowko Tsai <yowko@yowko.com>
# 對外使用 26379
EXPOSE 26379
# 將 sentinel.conf 複製至 container 的 /etc/redis/sentinel.conf
COPY sentinel.conf /etc/redis/sentinel.conf
# container 啟動時執行的指令
ENTRYPOINT redis-server /etc/redis/sentinel.conf --sentinel