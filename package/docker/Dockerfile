FROM ubuntu:24.04

ARG DEBIAN_FRONTEND=noninteractive
ARG MCNEEL_APT_DIST=stable
ARG DOTNET_VERSION=9.0.102

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
      ca-certificates \
      curl \
      gnupg \
      wget && \
    rm -rf /var/lib/apt/lists/*

RUN set -eux; \
    mkdir -p /etc/apt/keyrings; \
    curl -fsSL "https://mcneel-packages.s3.amazonaws.com/mcneel-packages.gpg.key" | \
      gpg --dearmor -o /etc/apt/keyrings/mcneel-packages.gpg; \
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/mcneel-packages.gpg] https://mcneel-packages.s3.amazonaws.com/deb ${MCNEEL_APT_DIST} main" \
      > /etc/apt/sources.list.d/mcneel-packages.list

RUN apt-get update && \
    apt-get install -y --no-install-recommends rhino-compute && \
    rm -rf /var/lib/apt/lists/*

RUN wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x ./dotnet-install.sh && \
    ./dotnet-install.sh --version ${DOTNET_VERSION} --install-dir /usr/share/dotnet && \
    rm -f ./dotnet-install.sh

ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="${DOTNET_ROOT}:${PATH}"

RUN cp /etc/rhino-compute/environment.example /etc/rhino-compute/environment

COPY docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

EXPOSE 80

ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]
CMD ["rhino-compute-start"]
