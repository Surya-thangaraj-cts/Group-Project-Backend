# CORS Error Fix - "Failed to fetch" Resolution

## 🔴 Error Message
```
Failed to fetch.
Possible Reasons:
- CORS
- Network Failure
- URL scheme must be "http" or "https" for CORS request.
```

---

## ✅ Current CORS Configuration (Verified)

Your backend already has CORS configured correctly:

```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Middleware order is correct:
app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
```

---

## 🔧 Solutions (Try in Order)

### **Solution 1: Verify Backend API URL**

Check what URL your Angular app is trying to call.

**In your Angular service (e.g., `report.service.ts`):**

```typescript
// ❌ WRONG - Will cause CORS error
private apiUrl = 'localhost:5000/api/reports';

// ✅ CORRECT
private apiUrl = 'http://localhost:5000/api/reports';
// OR
private apiUrl = 'https://localhost:5001/api/reports';
```

**Common mistakes:**
- Missing `http://` or `https://`
- Wrong port number
- Using `127.0.0.1` instead of `localhost`

---

### **Solution 2: Check Backend is Running**

1. **Start your .NET API:**
   ```bash
   dotnet run
   ```

2. **Verify API is accessible:**
   - Open browser: `https://localhost:5001/swagger`
   - If you see Swagger UI, API is running ✅

3. **Note the port number:**
   ```
   Look for: "Now listening on: https://localhost:5001"
   ```

---

### **Solution 3: Update CORS for Both HTTP and HTTPS**

If your API runs on both HTTP and HTTPS, update the CORS policy:

```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200"  // Add HTTPS if needed
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

---

### **Solution 4: Allow All Origins (Development Only)**

**⚠️ Use only for testing, NOT for production!**

```csharp
// In Program.cs - Temporary fix for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.AllowAnyOrigin()   // ⚠️ Dangerous in production
              .AllowAnyHeader()
              .AllowAnyMethod();
        // Note: Cannot use .AllowCredentials() with AllowAnyOrigin()
    });
});
```

If this works, the issue is with your origin configuration.

---

### **Solution 5: Check Angular Environment Configuration**

**In `environment.ts` or `environment.development.ts`:**

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api'  // ✅ Full URL with protocol
};
```

**In your service:**

```typescript
import { environment } from '../environments/environment';

@Injectable()
export class ReportService {
  private apiUrl = `${environment.apiUrl}/reports`;
  
  constructor(private http: HttpClient) {}
  
  getAllReports(): Observable<Report[]> {
    return this.http.get<Report[]>(this.apiUrl);
  }
}
```

---

### **Solution 6: Check launchSettings.json**

Verify your API's launch configuration:

**File: `Properties/launchSettings.json`**

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Note the URLs** and use them in your Angular app.

---

### **Solution 7: Disable HTTPS Redirection (Development Only)**

If HTTPS is causing issues:

```csharp
// In Program.cs
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // app.UseHttpsRedirection(); // Comment out for HTTP-only testing
}
else
{
    app.UseHttpsRedirection(); // Keep for production
}

app.UseCors("AllowAngular");
```

---

### **Solution 8: Check Browser Console**

Open browser DevTools (F12) and check:

1. **Network Tab:**
   - Look for the failed request
   - Check Request URL
   - Check Request Headers
   - Look for error details

2. **Console Tab:**
   - Look for specific CORS error messages
   - Example: "Access-Control-Allow-Origin header is missing"

---

## 📋 Complete Angular Service Example

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Report {
  id: number;
  reportType: string;
  title: string;
  totalTransactions: number;
  totalAmount: number;
  generatedAt: string;
  // ... other fields
}

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  // ✅ IMPORTANT: Include http:// or https://
  private apiUrl = 'https://localhost:5001/api/reports';

  constructor(private http: HttpClient) {}

  getAllReports(): Observable<Report[]> {
    return this.http.get<Report[]>(this.apiUrl);
  }

  getFilteredReports(params: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/filtered`, { params });
  }

  generateReport(data: any): Observable<Report> {
    return this.http.post<Report>(`${this.apiUrl}/generate`, data);
  }

  deleteReport(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
```

---

## 🧪 Quick Test

### **Test 1: Direct Browser Access**

Open browser and navigate to:
```
https://localhost:5001/api/reports
```

**Expected:** Either data or 401 Unauthorized (if auth required)  
**If you see:** Connection refused → API not running  

---

### **Test 2: Test with cURL**

```bash
# Test without authentication
curl -X GET "https://localhost:5001/api/reports" -k

# Test with authentication
curl -X GET "https://localhost:5001/api/reports" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -k
```

---

### **Test 3: Test with Postman**

1. Set URL: `https://localhost:5001/api/reports`
2. Add Header: `Authorization: Bearer {your-token}`
3. Send request
4. If this works → issue is in Angular configuration

---

## 🎯 Most Common Solutions

### **90% of CORS errors are fixed by:**

1. ✅ **Adding protocol to URL**
   ```typescript
   // ❌ Bad
   apiUrl = 'localhost:5001/api/reports'
   
   // ✅ Good
   apiUrl = 'https://localhost:5001/api/reports'
   ```

2. ✅ **Ensuring API is running**
   ```bash
   dotnet run
   # Look for: "Now listening on: https://localhost:5001"
   ```

3. ✅ **Using correct port**
   - Check `launchSettings.json`
   - Match in Angular service

---

## 🔍 Debugging Checklist

- [ ] Backend API is running (`dotnet run`)
- [ ] Can access Swagger at `https://localhost:5001/swagger`
- [ ] Angular app uses full URL with `http://` or `https://`
- [ ] Port numbers match between API and Angular
- [ ] CORS policy includes Angular origin (`http://localhost:4200`)
- [ ] CORS middleware is before Authentication/Authorization
- [ ] Browser console shows specific error
- [ ] Network tab shows the actual request URL

---

## 🚨 If Nothing Works

### **Nuclear Option - Reset Everything:**

1. **Stop both applications**
   ```bash
   # Stop API (Ctrl+C)
   # Stop Angular (Ctrl+C)
   ```

2. **Clear browser cache**
   - Chrome: Ctrl+Shift+Delete
   - Select "Cached images and files"

3. **Restart API**
   ```bash
   dotnet clean
   dotnet build
   dotnet run
   ```

4. **Verify API URL**
   - Note the exact URL from console
   - Example: `https://localhost:5001`

5. **Update Angular service**
   ```typescript
   private apiUrl = 'https://localhost:5001/api/reports';
   ```

6. **Restart Angular**
   ```bash
   ng serve
   ```

---

## 📞 Still Having Issues?

Share the following information:

1. **Backend console output** when starting the API
2. **Angular service code** (the file making the HTTP request)
3. **Browser console errors** (F12 → Console tab)
4. **Network tab details** (F12 → Network → Failed request)

---

## ✅ Success Indicators

You've fixed the issue when:

- ✅ No CORS errors in browser console
- ✅ Network tab shows 200 OK or 401 Unauthorized (if not logged in)
- ✅ Can see request headers with proper origin
- ✅ Response is received from API

---

**TL;DR: Most likely fix → Add `https://` to your API URL in Angular service!**
