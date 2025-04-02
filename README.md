# UserExe API

UserExe is a simple authentication and authorization API built with .NET 9 and ASP.NET Core Web API. It provides user registration, login, JWT authentication, and role-based access control.

## Features
- User registration and login with hashed passwords
- JWT authentication with access and refresh tokens
- Role-based authorization (Admin and User roles)
- Secure API endpoints with authentication and authorization

## Technologies Used
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server (with Docker support)
- JWT Authentication
- Dependency Injection

## Getting Started

### Prerequisites
- .NET 9 SDK installed
- SQL Server running (or using Docker)
- JetBrains Rider (or any C# IDE)
- Git (for version control)

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/UserExe.git
   cd UserExe
   ```
2. Configure the database connection in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=UserExeDB;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
3. Apply database migrations:
   ```bash
   dotnet ef database update
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

## API Endpoints

### Authentication
- **Register:** `POST /api/auth/register`
- **Login:** `POST /api/auth/login`
- **Refresh Token:** `POST /api/auth/refresh-token`

### Protected Endpoints
- **Authenticated Users:** `GET /api/auth`
- **Admin-Only:** `GET /api/auth/admin-only` (Requires Admin role)

## Testing
Use tools like **Postman** or **Swagger** to test the API endpoints.

## Contributing
Feel free to fork this repository and submit pull requests with improvements or bug fixes.

## License
This project is open-source and available under the MIT License.
