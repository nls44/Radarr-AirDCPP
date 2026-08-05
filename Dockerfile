ARG ALPINE_VERSION="3.21.3"

FROM alpine:${ALPINE_VERSION} AS base

ARG TARGETPLATFORM
ARG RADARR_RELEASE="latest"
ENV TARGETPLATFORM=${TARGETPLATFORM:-linux/amd64}

RUN \
  echo "**** install packages ****" && \
  apk add --no-cache \
    curl \
    jq \
    bash \
    ca-certificates \
    coreutils \
    icu-libs \
    sqlite-libs \
    xmlstarlet && \
  echo "**** install radarr ****" && \
  echo "TARGETPLATFORM=$TARGETPLATFORM" && \
  RELEASE_FOR_PLATFORM=$(case ${TARGETPLATFORM:-linux/amd64} in \
    "linux/amd64")   echo ".*linux-musl-x64.tar.gz*"  ;; \
    "linux/arm64")   echo ".*linux-musl-arm64.tar.gz*" ;; \
    *)               echo ""        ;; esac) && \
  echo "RELEASE_FOR_PLATFORM=$RELEASE_FOR_PLATFORM" && \
  mkdir -p \
   /app/radarr/bin \
   /config && \
  curl -o \
    /tmp/radarr.tar.gz -L \
	"$(curl -s "https://api.github.com/repos/nls44/Radarr-AirDCPP/releases/${RADARR_RELEASE}" | jq '.assets' | jq -r --arg RELEASE_FOR_PLATFORM "$RELEASE_FOR_PLATFORM" '.[].browser_download_url | match($RELEASE_FOR_PLATFORM;"i") | .string')" && \
  tar xzf \
    /tmp/radarr.tar.gz -C \
    /app/radarr/bin --strip-components=1 && \
  chmod +x /app/radarr/bin/Radarr && \
  echo "**** cleanup ****" && \
  rm -rf \
    /app/radarr/bin/Radarr.Update \
    /tmp/*

# ports and volumes
EXPOSE 8989

VOLUME /config

CMD ["/app/radarr/bin/Radarr", "-nobrowser", "-data=/config"]