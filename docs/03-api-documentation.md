# API Documentation

## 🌐 API Overview

The Travel Accommodation Booking Platform provides a comprehensive RESTful API built with ASP.NET Core. The API follows REST principles and includes features like JWT authentication, rate limiting, caching, and comprehensive error handling.

### Base URL
- **Development**: `http://localhost:5000`
- **Production**: `https://your-domain.com`

### API Versioning
The API supports versioning through URL path:
- Current version: `v1`
- Example: `/api/v1/auth/login`

### API Documentation
- **[APIDog](https://gggxdms4n2.apidog.io)**

## 🔧 API Features

### Caching & Performance
- **ETag Support**: All GET endpoints support ETag caching
- **Memory Caching**: Frequently accessed data is cached
- **Response Compression**: Automatic response compression

### Rate Limiting
- **Default Limit**: 10 requests per minute per IP
- **Configurable**: Limits can be adjusted per endpoint
- **Headers**: Rate limit information in response headers

### Error Handling
All endpoints return consistent error responses:

```json
{
  "message": "Error description",
  "details": "Additional error details (in development)"
}
```

### HTTP Status Codes
- `200 OK`: Successful GET/PATCH requests
- `201 Created`: Successful POST requests
- `204 No Content`: Successful DELETE requests
- `304 Not Modified`: Cached content is still valid
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `409 Conflict`: Resource conflict (e.g., duplicate email)
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: Server error

### Security Headers
All responses include security headers:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: no-referrer`

## 📝 Data Validation

### Request Validation
All write operations include comprehensive validation:
- **Required Fields**: Marked with `[Required]` attribute
- **String Length**: Min/max length validation
- **Email Format**: Valid email address format
- **Date Format**: ISO 8601 date format
- **Range Validation**: Numeric ranges for ratings, prices, etc.

### Response Format
All responses follow consistent JSON structure with camelCase naming convention.

---

**Continue to**: [Database Documentation](04-database.md) for detailed schema information.
