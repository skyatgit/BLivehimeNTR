# BLivehimeNTR

B站直播姬NTR工具

在本地开启TCP和WebSocket服务器，然后从直播姬获取弹幕数据解压、重新封装并转发

Tcp服务器在 localhost:19981 上启动

WebSocket服务器在 ws://localhost:19980/BLiveSMS/ 上启动

转发的数据内容基本和原数据格式一致 原数据文档: https://open-live.bilibili.com/document/657d8e34-f926-a133-16c0-300c1afc6e6b

但由于是解压后的数据,而且本地心跳逻辑稍有变动,导致与原版有如下差异(仅对原版逻辑进行删减,无变更内容,所以可以完全兼容原版逻辑)

    Version恒定为0,不再会有被压缩的数据
    Operation为
        OP_HEARTBEAT_REPLY	3	本地服务器主动发送的心跳包
        OP_SEND_SMS_REPLY	5	本地服务器推送的弹幕消息包
    客户端不需要向本地服务器发送心跳包
    客户端不需要向本地服务器发送鉴权包
    本地服务器不会回复鉴权包