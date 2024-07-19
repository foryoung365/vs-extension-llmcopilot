# FAQ

## Does `LLMCopilot` send code to other places?

Absolutely not. `LLMCopilot` only communicates with the `Ollama` server you configure. All data is transferred between your machine and the `Ollama` server you deploy. Data is not sent anywhere else.

## Which files are used as references for code auto-completion?

In the current version, only the file being edited is used as a reference for code auto-completion.

## What is the length of the `Context Window` used, and how can it be modified?

- The default `2K Context Window` is used for code completion, while the `4K Context Window` is used for chat.
- You can make modifications in the setting page of the extension.

## What is the `Reverse Proxy Access Token` in the `Settings` page? How do I use it?

Sometimes you may want to expose the `Ollama` server to the internet but wish to protect it using a reverse proxy like `nginx`. In such cases, you might need an `Access Token` to verify that requests are from a trusted source. You can set the `Reverse Proxy Access Token` to include your `Access Token`.

When you set the `Reverse Proxy Access Token`, `LLMCopilot` will add the `Authorization: Bearer <your_token>` header to requests made to `Ollama`. This allows you to verify the legitimacy of requests on the reverse proxy server.
Please note that this setting is only relevant for users using a reverse proxy. If you access the `Ollama` server directly, you do not need to add this token.

Below is an example `nginx` configuration provided by `chatGPT` to validate requests:
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
