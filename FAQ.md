# FAQ

## `LLMCopilot`是否会将代码发送到其他地方？

绝对不会，LLMCopilot仅会与你配置`Ollama`服务器通信，所有的数据都在你的机器和你部署的`Ollama`服务器之间进行传输。数据不会被发送到任何其他地方。

## 哪些文件会作为代码自动完成的参考？

目前版本中，仅当前编辑文件会作为代码自动完成的参考。

## 使用的`Context Window`长度是多少，如何修改？

- 默认的`2K Context Window`用于代码完成，`4K Context Window`用于对话。
- 你可以在插件的配置页中进行修改。

## `Settings`设置页中的`Reverse Proxy Access Token`是什么？如何使用？

有时你可能想要将`Ollama`服务器暴露在互联网上，但是又希望使用类似`nginx`的反向代理来保护它。这时候你可能需要一个`Access Token`来验证请求是否来自可信任的源。这时你可以设置`Reverse Proxy Access Token`来添加你的`Access Token`。
当你设置了`Reverse Proxy Access Token`后，`LLMCopilot`会在`Ollama`请求中添加`Authorization: Bearer <your_token>`头部信息。这样你就可以在反向代理服务器上验证请求的合法性了。
请注意，这个设置只对使用反向代理的用户有效，如果你直接访问`Ollama`服务器，不需要添加此令牌。

下面是`chatGPT`提供的一个`nginx`配置示例，用于验证请求的合法性：
```
http {
    map $http_authorization $token {
        default "deny";
        "Bearer your_secret_token" "allow";
    }

    server {
        listen 80;
        server_name your_domain;

        location / {
            if ($token = "deny") {
                return 401;
            }
            proxy_pass http://localhost:11434;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        listen 443 ssl; # managed by Certbot
        ssl_certificate /etc/letsencrypt/live/your_domain/fullchain.pem; # managed by Certbot
        ssl_certificate_key /etc/letsencrypt/live/your_domain/privkey.pem; # managed by Certbot
        include /etc/letsencrypt/options-ssl-nginx.conf; # managed by Certbot
        ssl_dhparam /etc/letsencrypt/ssl-dhparams.pem; # managed by Certbot
    }
}
```
