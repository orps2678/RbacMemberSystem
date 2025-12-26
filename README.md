# RbacMemberSystem 開發環境

## 快速啟動

### 1. 啟動 PostgreSQL
```bash
docker-compose up -d
```

### 2. 確認運行狀態
```bash
docker-compose ps
```

### 3. 停止服務
```bash
docker-compose down
```

### 4. 停止並刪除資料
```bash
docker-compose down -v
```

## 連線資訊

| 項目 | 值 |
|------|-----|
| Host | localhost |
| Port | 5432 |
| Database | RbacMemberDb |
| Username | postgres |
| Password | postgres |

## Connection String
```
Host=localhost;Port=5432;Database=RbacMemberDb;Username=postgres;Password=postgres
```