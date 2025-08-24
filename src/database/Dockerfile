# Use PostgreSQL 17 official image
FROM postgres:17

# Set environment variables for PostgreSQL
ENV POSTGRES_DB=scs-demo
ENV POSTGRES_USER=postgres
ENV POSTGRES_PASSWORD=Test@123
ENV TZ=Asia/Ho_Chi_Minh
ENV PGTZ=Asia/Ho_Chi_Minh

# Set the default locale
ENV LANG=en_US.utf8

# Expose PostgreSQL port
EXPOSE 5432

# Create a directory for initialization scripts
RUN mkdir -p /docker-entrypoint-initdb.d

# Copy seed data to the container for automatic initialization
COPY scs_seed_data.sql /docker-entrypoint-initdb.d/

# Set the default command to run PostgreSQL
CMD ["postgres"]
