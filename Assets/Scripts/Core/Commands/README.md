```
Enqueue(cmd1) → Enqueue(cmd2) → ExecuteAll()
                                    │
                                    ▼
                            ┌───────────────┐
                            │ ExecuteNext() │
                            └───────┬───────┘
                                    │
                    ┌───────────────┴───────────────┐
                    ▼                               ▼
            Queue empty?                    Dequeue cmd1
                    │                               │
                    ▼                               ▼
            OnQueueCompleted            cmd1.Execute(callback)
                                                    │
                                                    ▼
                                            OnComplete callback
                                                    │
                                                    ▼
                                            ExecuteNext() → cmd2...
```
