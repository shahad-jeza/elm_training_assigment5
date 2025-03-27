# E-Commerce System API

A mini e-commerce platform built with .NET Core featuring:
- User authentication with JWT
- Product management
- Order processing
- Role-based access control

## Features

✅ **User Authentication**  
🔐 JWT token-based authentication  
👥 Role-based authorization (Admin/Customer)  

✅ **Product Management**  
📦 Admin CRUD operations  
🔍 Product search and filtering  

✅ **Order Processing**  
🛒 Create and view orders  
📊 Order status tracking  

## Tech Stack

- **Backend**: .NET 8
- **Database**: SQLite
- **Authentication**: JWT Bearer Tokens
- **Testing**: xUnit + Moq

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQLite (included)

### Installation
1. Clone the repository
   ```bash
   git clone https://github.com/yourusername/ecommerce-system.git

2. Navigate to project directory
   ```bash
      cd ECommerceSystem
3. Restore dependencies
    ```bash
    dotnet restore
4. Running the API
    ```bash
      dotnet run
