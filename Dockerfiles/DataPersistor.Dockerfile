FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine
WORKDIR /app
COPY ./app .
EXPOSE 2501
ENTRYPOINT [ "DataPersistor" ]