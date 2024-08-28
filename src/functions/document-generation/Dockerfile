# Uses the official dotnet 8.0 sdk image to build the binaries
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 as build

WORKDIR /src

# Copy csproj files only as that is all we need to restore nuget packages
# this speeds up local build time as the restore step won't run if these files haven't changed
COPY ["DocumentGeneration.Common/DocumentGeneration.Common.csproj", "DocumentGeneration.Common/DocumentGeneration.Common.csproj"]
COPY ["DocumentGeneration.Lambdas/DocumentGeneration.Lambdas.csproj", "DocumentGeneration.Lambdas/DocumentGeneration.Lambdas.csproj"]

# restore nuget packages at this point, if the csproj files have not changed, this step will be skipped
# which is what we want
RUN dotnet restore "DocumentGeneration.Lambdas/DocumentGeneration.Lambdas.csproj"

# Copy our files from the current directory on our machine "." to the current WORKDIR "." (which is /src in our case)
COPY . .

# Build the main project
WORKDIR /src/DocumentGeneration.Lambdas
RUN dotnet build "DocumentGeneration.Lambdas.csproj" \
    --no-restore \
    --runtime linux-x64 \
    --configuration Release \
    --output /app/build

# Publish the main project
FROM build as publish
RUN dotnet publish "DocumentGeneration.Lambdas.csproj" \
            --configuration Release \
            --runtime linux-x64 \
            --output /app/publish

# Prepare our deployment image
FROM --platform=linux/amd64 public.ecr.aws/lambda/dotnet:8.2024.08.09.13

WORKDIR ${LAMBDA_TASK_ROOT}

# install necessary packages
RUN dnf update -y
RUN dnf install -y \
        gcc-c++ \
        pango.x86_64 \
        libXcomposite.x86_64 \
        libXcursor.x86_64 \
        dbus-glib-devel \
        libXdamage.x86_64 \
        libXi.x86_64 \
        libXtst.x86_64 \
        cups-libs.x86_64 \
        libXScrnSaver.x86_64 \
        libXrandr.x86_64 \
        alsa-lib.x86_64 \
        atk.x86_64 \
        gtk3.x86_64 \
        ipa-gothic-fonts \
        xorg-x11-fonts-100dpi \
        xorg-x11-fonts-75dpi \
        xorg-x11-utils \
        xorg-x11-fonts-cyrillic \
        xorg-x11-fonts-Type1 \
        xorg-x11-fonts-misc \
        mesa-libgbm.x86_64 \
        nss-3.90.0-3.amzn2023.0.4.x86_64

# Copy our published files to our current WORKDIR "."
COPY --from=publish /app/publish .
CMD [ "DocumentGeneration.Lambdas::DocumentGeneration.Lambdas.GeneratorFunction::DocumentGeneratorHandler" ]
