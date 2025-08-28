
## Project Overview

Developed using **ASP.NET Core**, this project provides a secure API for SMS integration. It enables users to interact with an external SMS service through defined routes, requiring a **username** and **password** for authentication. The project serves as both an example of how to integrate with an SMS API and how to create your own API that consumes an external SMS service using custom credentials.

Additionally, the system implements strict logging practices. Logs are stored in the structured format:
`/foldername/yy/mm/dd`,
ensuring organized, date-based log management for easy access, debugging, and monitoring.

---


Example API Request

## Example API Request

To interact with the SMS service, make a POST request to:

 http://localhost:5000/sms/onesms

**Request Body:**
```json
{
  "timestamp": "2025-08-21T10:30:00Z",
  "phoneNumber": "0912345678",
  "userName": "john.doe",
  "password": "*************************",
  "message": "Hello, this is a test message",
  "language": "EN"
}
