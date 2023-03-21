FROM ghcr.io/architecture-it/net:3.1-sdk as build
WORKDIR /app
COPY . .
RUN dotnet restore
WORKDIR "/app/src/service"
RUN dotnet build "AccionesBBVA.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccionesBBVA.csproj" -c Release -o /app/publish

FROM ghcr.io/architecture-it/net:3.1
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AccionesBBVA.dll"]


#FROM ghcr.io/architecture-it/net:6.0-sdk as build
#WORKDIR /app
#COPY . .
#RUN dotnet restore
#WORKDIR "/app/src/service"
#RUN dotnet build "dotnet_gestion_transportista_cron.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "dotnet_gestion_transportista_cron.csproj" -c Release -o /app/publish
#
#FROM ghcr.io/architecture-it/net:6.0
#COPY --from=publish /app/publish .
#
#ENTRYPOINT ["dotnet", "dotnet_gestion_transportista_cron.dll"]
