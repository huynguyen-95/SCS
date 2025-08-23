# SCS
Smart City Surveillance

Demo URL: http://13.212.200.185/scs

Demo URL will be available until 31-10-2025.

Users:
1. 88907299 - Admin
2. 88900001 - SCS User

## Development Setup

### Prerequisites
- Docker and Docker Compose installed on your machine
- Git for version control

### Running the Application Locally

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Smart-City-Surveillance
   ```

2. **Navigate to the source directory**
   ```bash
   cd src
   ```

3. **Configure environment variables**
   - Copy the `.env` file and update the values with your actual configuration:
   ```bash
   # AWS Configuration
   AWS_REGION=us-east-1
   AWS_ACCESS_KEY=your_aws_access_key_here
   AWS_SECRET_KEY=your_aws_secret_key_here
   AWS_QUEUE_URL=your_sqs_queue_url_here
   AWS_BUCKET_NAME=your_s3_bucket_name_here
   
   # Database Configuration
   CONNECTION_STRING=your_database_connection_string_here
   ```

4. **Start the application with Docker Compose**
   ```bash
   # Start services in detached mode (background)
   docker-compose up -d
   
   # Or start services with logs visible
   docker-compose up
   ```

5. **Access the application**
   - **Frontend (Angular)**: http://localhost:4200
   - **Backend API**: http://localhost:8080

6. **Stop the application**
   ```bash
   docker-compose down
   ```

### Services
- **scs-api**: .NET Core API backend
- **scs-client**: Angular frontend served by Nginx

### Development Notes
- The application runs in Development mode by default
- Both services are connected through a custom Docker network
- The frontend depends on the backend and will wait for it to start
- Make sure to configure your environment variables before running
