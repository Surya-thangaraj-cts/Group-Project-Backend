# JWT 401 Unauthorized Error - Debugging Guide

## ❌ Problem
Getting **401 Unauthorized** error even when providing a valid JWT token in the Authorization header.

## 🔍 Common Causes

### 1. **Token Format Issue**
- ❌ Wrong: `Authorization: <token>`
- ❌ Wrong: `Authorization: bearer <token>` (lowercase)
- ✅ Correct: `Authorization: Bearer <token>` (with capital B and space)

### 2. **Token Expiry**
- Tokens expire after 120 minutes (2 hours) by default
- Check if your token is expired

### 3. **Issuer/Audience Mismatch**
- Token must have matching `iss` and `aud` claims
- Check your appsettings.json values

### 4. **Signing Key Mismatch**
- The key used to sign the token must match validation key
- If you changed the key, old tokens won't work

### 5. **Clock Skew Issues**
- Server/client clock differences
- Already set to 5 minutes tolerance

### 6. **Claims Mapping Issues**
- Role claim must be "role" (lowercase)
- Name claim must be "name" (lowercase)

---

## 🛠️ Quick Fixes

### Fix 1: Verify Token Format in Swagger

When testing in Swagger UI:

1. Click **Authorize** button (padlock icon)
2. Enter token in this exact format:
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
3. Click **Authorize**
4. Click **Close**
5. Try your API call again

**Note:** Swagger automatically adds "Bearer" prefix, so just paste the token!

### Fix 2: Check Token Claims

Decode your JWT token at https://jwt.io and verify:

```json
{
  "sub": "admin",
  "nameid": "admin",
  "name": "System Admin",
  "email": "admin@example.com",
  "role": "Admin",
  "status": "Active",
  "iss": "Company",
  "aud": "FrontendApp",
  "exp": 1739601234  // ← Check this isn't in the past!
}
```

### Fix 3: Get a Fresh Token

1. Call `/api/auth/login` with:
   ```json
   {
     "userId": "admin",
     "password": "Admin@123!"
   }
   ```

2. Copy the token from response:
   ```json
   {
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
     "user": { ... }
   }
   ```

3. Use this fresh token in Authorization header

### Fix 4: Add Enhanced Logging

Add this to your `Program.cs` BEFORE `var app = builder.Build();`:

```csharp
// Add this after JWT configuration
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

This will show detailed JWT authentication logs.

---

## 🔧 Step-by-Step Debugging

### Step 1: Test Login Endpoint

1. Open Swagger UI: `https://localhost:7021/swagger`
2. Find `/api/auth/login`
3. Click "Try it out"
4. Use these credentials:
   ```json
   {
     "userId": "admin",
     "password": "Admin@123!"
   }
   ```
5. You should get a 200 response with a token

### Step 2: Test Protected Endpoint

1. Copy the token from Step 1
2. Click the **Authorize** button at the top
3. Paste token (Swagger adds "Bearer " automatically)
4. Click **Authorize** then **Close**
5. Try any protected endpoint (e.g., `/api/admin/pending-users`)

**Expected:** ✅ 200 OK  
**If you get 401:** Continue to Step 3

### Step 3: Check Console Logs

Look for these messages in your VS Output window (Debug):

✅ **Good signs:**
```
JWT Token Validated. Claims: sub=admin, name=System Admin, email=admin@example.com, role=Admin
```

❌ **Bad signs:**
```
JWT Auth Failed: IDX10223: Lifetime validation failed...
JWT Auth Failed: IDX10214: Audience validation failed...
JWT Challenge: invalid_token
```

### Step 4: Common Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| `IDX10223: Lifetime validation failed` | Token expired | Get a new token |
| `IDX10214: Audience validation failed` | Audience mismatch | Check appsettings.json |
| `IDX10205: Issuer validation failed` | Issuer mismatch | Check appsettings.json |
| `IDX10503: Signature validation failed` | Wrong signing key | Key was changed |
| No JWT logs at all | Token not sent properly | Check Authorization header format |

---

## 🎯 Specific Fixes for Your Setup

### Your Current Configuration:

```json
// appsettings.json
{
  "Jwt": {
    "Issuer": "Company",
    "Audience": "FrontendApp",
    "Key": "a-very-long-strong-secret-key-change-me-must-be-at-least-64-chars-long!!",
    "ExpiresMinutes": 120
  }
}
```

### Token Must Have:
- ✅ `iss: "Company"`
- ✅ `aud: "FrontendApp"`
- ✅ Signed with the key above
- ✅ `exp` (expiry) in the future

---

## 🐛 Advanced Debugging

### Add Breakpoint in AuthController

1. Open `Controllers/AuthController.cs`
2. Set breakpoint on line where `_jwt.CreateToken(user)` is called
3. Run in debug mode
4. Login and inspect the token being created

### Verify Token Generation

Add this temporary code after token creation in `AuthController.cs`:

```csharp
var token = _jwt.CreateToken(user);

// Temporary debug code
System.Console.WriteLine($"=== TOKEN GENERATED ===");
System.Console.WriteLine($"Token: {token}");
System.Console.WriteLine($"==================");

return Ok(new { token, user = ... });
```

Copy the token from console and decode it at jwt.io.

---

## ✅ Working Example

### 1. Login Request:
```http
POST /api/auth/login
Content-Type: application/json

{
  "userId": "admin",
  "password": "Admin@123!"
}
```

### 2. Login Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsIm5hbWVpZCI6ImFkbWluIiwibmFtZSI6IlN5c3RlbSBBZG1pbiIsImVtYWlsIjoiYWRtaW5AZXhhbXBsZS5jb20iLCJyb2xlIjoiQWRtaW4iLCJzdGF0dXMiOiJBY3RpdmUiLCJleHAiOjE3Mzk2MDEyMzQsImlzcyI6IkNvbXBhbnkiLCJhdWQiOiJGcm9udGVuZEFwcCJ9.xyz123",
  "user": {
    "userId": "admin",
    "name": "System Admin",
    "email": "admin@example.com",
    "branch": "HQ",
    "role": "Admin",
    "status": "Active"
  }
}
```

### 3. Protected Request:
```http
GET /api/admin/pending-users
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsIm5hbWVpZCI6ImFkbWluIiwibmFtZSI6IlN5c3RlbSBBZG1pbiIsImVtYWlsIjoiYWRtaW5AZXhhbXBsZS5jb20iLCJyb2xlIjoiQWRtaW4iLCJzdGF0dXMiOiJBY3RpdmUiLCJleHAiOjE3Mzk2MDEyMzQsImlzcyI6IkNvbXBhbnkiLCJhdWQiOiJGcm9udGVuZEFwcCJ9.xyz123
```

**Expected Response:** ✅ 200 OK with user data

---

## 🔥 Nuclear Option: Reset Everything

If nothing works:

1. **Stop the application**
2. **Delete the database**:
   ```sql
   DROP DATABASE TestDb;
   ```
3. **Clean and rebuild**:
   ```powershell
   dotnet clean
   dotnet build
   ```
4. **Run the app** - migrations will recreate DB
5. **Login again** to get a fresh token
6. **Test with the new token**

---

## 📊 Checklist

Before asking for help, verify:

- [ ] I logged in successfully and got a token
- [ ] I copied the FULL token (including all dots)
- [ ] I'm using `Bearer <token>` format (capital B)
- [ ] My token is not expired (check at jwt.io)
- [ ] The endpoint requires `[Authorize]` attribute
- [ ] I can see JWT logs in the console
- [ ] My appsettings.json has correct Jwt configuration
- [ ] I restarted the app after configuration changes

---

## 🆘 Still Getting 401?

If you've tried everything above and still getting 401:

1. **Copy the EXACT token you're using**
2. **Decode it at https://jwt.io**
3. **Screenshot the decoded payload**
4. **Check the `exp` (expiry) claim** - is it in the past?
5. **Check the `iss` and `aud` claims** - do they match appsettings.json?
6. **Share the decoded payload** (without the signature part)

---

## 📅 Date: February 15, 2026
**Status**: Debug Guide Created

---

## Quick Test Script

Use this in PowerShell to test:

```powershell
# 1. Login and get token
$loginResponse = Invoke-RestMethod -Uri "https://localhost:7021/api/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body '{"userId":"admin","password":"Admin@123!"}' `
    -SkipCertificateCheck

$token = $loginResponse.token
Write-Host "Token: $token"

# 2. Test protected endpoint
$headers = @{
    "Authorization" = "Bearer $token"
}

$users = Invoke-RestMethod -Uri "https://localhost:7021/api/admin/pending-users" `
    -Method GET `
    -Headers $headers `
    -SkipCertificateCheck

Write-Host "Success! Got $($users.Count) users"
```

If this script works but Swagger doesn't, the issue is with how you're using Swagger!
