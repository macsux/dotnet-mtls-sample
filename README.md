This sample demonstrates how to do MTLs for web server with .NET 5 for both server and client components. Two modes are shown. 

1. SSL cert is terminated at load balancer and sent to the web server as plain http. The original cert is is available as HTTP header.
2. Web server (Kestrel) handles TLS handshake itself. 