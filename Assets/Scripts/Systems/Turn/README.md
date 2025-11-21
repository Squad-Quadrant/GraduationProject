```
StartTurn() -> NextUnit() -> [单位行动] -> EndUnitTurn() -> 队列是否为空? -> Yes: EndTurn() -> NextTurn() 
                  ^                                             | No
                  |                                             |
                  -----------------------------------------------
```
