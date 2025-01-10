# Exchange server

## Commands

- Migrate database
```bash
  ObjectExchange -migrate
```
- grand admin access for user (if admin exist, not grand access)
```bash
  ObjectExchange -grant[username])
```
- create demo
	```bash
	ObjectExchange -demo
	```
    a. Create demo user <b>DemoUser@contoso.com</b> with password <b>DemoPassword1!</b>
    b. Create demo client <b>DemoClient</b> associated with user <b>DemoUser@contoso.com</b>
    c. Create demo nodes <b>FirstDemoNode</b> and <b>SecondDemoNode</b> associated with client <b>DemoClient</b>
