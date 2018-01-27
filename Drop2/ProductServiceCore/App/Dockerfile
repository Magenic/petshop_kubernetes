FROM gcr.io/google-appengine/aspnetcore:2.0
COPY . /app
ENV ASPNETCORE_URLS=http://*:${PORT}
WORKDIR /app
ENTRYPOINT ["dotnet", "ProductServiceCore.dll"]