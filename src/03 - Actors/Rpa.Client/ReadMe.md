## Dapr ��������
```bash
dapr run --app-id rpaclient --app-port 5001 --app-protocol http --dapr-http-port 56001 --resources-path ./components -- dotnet run --urls=http://localhost:5001/
```

## `statestore.yaml` �ļ��� redis �˿�Ҫ�� Dapr__redis�Ķ˿�һ��


## �鿴��ʱ����¼
http://localhost:56001/v1.0/actors/RpaActor/0197c1cb-53fc-7c28-be3c-66d505f938f6/reminders/taskShouldBeFailed
http://localhost:<Dapr-Http-Port>/v1.0/actors/<Actor-Type>/<Actor-Id>/reminders/<Reminder-Name>