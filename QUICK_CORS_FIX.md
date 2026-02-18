## 🚀 QUICK FIX - CORS Error Resolution

### ⚡ **FASTEST FIX (90% of cases)**

**In your Angular service file (e.g., `report.service.ts`):**

```typescript
// ❌ WRONG - Missing protocol
private apiUrl = 'localhost:5001/api/reports';

// ✅ CORRECT - Full URL with https://
private apiUrl = 'https://localhost:5001/api/reports';
```

---

### 🔧 **CORS Configuration Updated**

Your backend CORS is now configured to accept both HTTP and HTTPS:

```csharp
// ✅ Updated in Program.cs
policy.WithOrigins(
    "http://localhost:4200",   // HTTP
    "https://localhost:4200"   // HTTPS
)
```

---

### 📝 **Step-by-Step Fix**

1. **Start your backend API:**
   ```bash
   dotnet run
   ```
   
2. **Note the URL shown in console:**
   ```
   Now listening on: https://localhost:5001
   ```

3. **Update your Angular service:**
   ```typescript
   private apiUrl = 'https://localhost:5001/api/reports';
   //                ^^^^^^^ Use this exact URL
   ```

4. **Restart Angular:**
   ```bash
   ng serve
   ```

---

### ✅ **Verification**

Test API is accessible:
```
Open browser → https://localhost:5001/swagger
```

If Swagger loads → API is running ✅

---

### 📋 **Common Mistakes**

| ❌ Wrong | ✅ Correct |
|---------|-----------|
| `localhost:5001` | `https://localhost:5001` |
| `http://localhost:5001` (when API uses HTTPS) | `https://localhost:5001` |
| `127.0.0.1:5001` | `https://localhost:5001` |
| Wrong port (e.g., 5000) | Check actual port in console |

---

### 🆘 **If Still Not Working**

1. **Check Browser Console (F12):**
   - Look for actual error message
   - Check Network tab for request details

2. **Verify API is running:**
   ```bash
   curl -k https://localhost:5001/api/reports
   ```

3. **Try allowing all origins (TEMPORARILY):**
   ```csharp
   // In Program.cs - FOR TESTING ONLY
   policy.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod();
   ```

---

### 📞 **Need More Help?**

See full guide: **CORS_ERROR_FIX.md**
