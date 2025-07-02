# My first docker build

# This line specifies the base image for our Docker image.
# 'debian:bullseye' means we are using the Debian operating system,
# specifically the 'bullseye' release, as the foundation for our container.
FROM debian:bullseye

# This instruction sets the working directory inside the container.
# All subsequent commands (like COPY, RUN, CMD) will be executed relative to this directory.
# If the directory doesn't exist, Docker will create it.
WORKDIR /app

# This section is for installing Node.js within the Debian base image.
# RUN commands execute instructions in a new layer on top of the current image.
RUN apt-get update && \
    # apt-get update: Updates the package list for the Debian system.
    # &&: This is a shell operator that ensures the next command only runs if the previous one was successful.
    apt-get install -y curl && \
    # apt-get install -y curl: Installs the 'curl' utility, which is used to download files from the internet.
    # -y: Automatically answers 'yes' to prompts from apt-get.
    curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    # curl -fsSL ... | bash -: Downloads the NodeSource setup script for Node.js version 22.x
    # and pipes it directly to 'bash -' to execute it.
    # -f: Fail silently (don't show error message on HTTP errors).
    # -s: Silent mode (don't show progress meter or error messages).
    # -S: Show error (show error message if silent mode is used and it fails).
    # -L: Follow redirects.
    apt-get install -y nodejs
    # apt-get install -y nodejs: Installs Node.js itself, which was made available by the NodeSource script.

# This instruction copies files from the build context (the directory where you run 'docker build')
# into the current working directory inside the container (/app, as set by WORKDIR).
# The first '.' refers to the source path on your local machine.
# The second '.' refers to the destination path inside the container.
COPY . .

# This instruction informs Docker that the container will listen on the specified network port at runtime.
# It's documentation for users and other Docker tools, but doesn't actually publish the port.
# To access the port from the host, you need to publish it using the '-p' flag with 'docker run'.
EXPOSE 3000

# This instruction provides the default command to execute when the container starts.
# It's an executable array form, which is the preferred way as it's less ambiguous.
# It tells the container to run 'node' and execute 'server.js' as its argument.
CMD ["node" ,"server.js"]

# For creating Image
# docker build -t my-node-app .
# This is a shell command to build the Docker image.
# 'docker build': The command to initiate a Docker image build.
# '-t my-node-app': Tags the resulting image with the name 'my-node-app'. You can replace 'my-node-app'
#                  with any desired name. This makes it easier to refer to the image later.
# '.': Specifies the build context. The '.' means the current directory, indicating that Docker should
#      look for the Dockerfile and any files referenced by COPY instructions in the current directory.
