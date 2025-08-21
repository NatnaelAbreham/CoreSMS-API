Developed using ASP.NET Core, this project provides an API for SMS integration. It allows users to interact with an external SMS service via a secure API. The API requires a username and password for authentication and provides access to the SMS service through defined routes. This project demonstrates how to integrate with an SMS API and how to create your own API that can consume the integrated SMS service using custom credentials.

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
