## Dapr ��������
```bash
dapr run --app-id rpaclient --app-port 5001 --app-protocol http --dapr-http-port 56001 --resources-path ./components -- dotnet run --urls=http://localhost:5001/
```

## `statestore.yaml` �ļ��� redis �˿�Ҫ�� Dapr__redis�Ķ˿�һ��
