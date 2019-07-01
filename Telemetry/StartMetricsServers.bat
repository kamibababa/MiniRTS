start "Grafana" "%~dp0Grafana\bin\grafana-server.exe" --homepath "%~dp0Grafana/"
start "Prometheus" "%~dp0Prometheus\prometheus.exe" --config.file="%~dp0Prometheus/prometheus.yml" --storage.tsdb.path="%~dp0Prometheus/data/"