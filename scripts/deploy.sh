#!/usr/bin/env bash
# Snabb-deploy för STR Demo / bokning-prototypen.
#
# Kör: ./scripts/deploy.sh "commit-meddelande"
# (Om inget meddelande anges föreslås ett baserat på datum.)
#
# Skriptet:
#   1. Visar vad som kommer commit:as
#   2. Frågar om bekräftelse
#   3. Kör git add -A, commit och push
#   4. Railway plockar upp pushen och deployar automatiskt
#
# Repot finns på Railway-pipelinen — push till origin/main = live deploy.

set -e

# Färger för läsbar output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No color

# Gå till repo-roten (parent till scripts/-mappen) oavsett varifrån skriptet körs
REPO_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )/.." && pwd )"
cd "$REPO_ROOT"

# Rensa eventuell stale lock-fil (kan ligga kvar om en tidigare git-process avbröts)
if [ -f .git/index.lock ]; then
    echo -e "${YELLOW}⚠ Hittade stale .git/index.lock — tar bort den.${NC}"
    rm -f .git/index.lock
fi

# Verifiera att vi är på en branch som går att pusha
BRANCH=$(git rev-parse --abbrev-ref HEAD)
echo -e "${BLUE}● Branch:${NC} $BRANCH"

# Visa status
echo ""
echo -e "${BLUE}● Ändringar att committa:${NC}"
git status --short
echo ""

# Avbryt om inget att committa
if [ -z "$(git status --porcelain)" ]; then
    echo -e "${YELLOW}Inga ändringar att committa. Avbryter.${NC}"
    exit 0
fi

# Commit-meddelande från argument, annars default
if [ -n "$1" ]; then
    MSG="$1"
else
    MSG="Uppdatering $(date +'%Y-%m-%d %H:%M')"
    echo -e "${YELLOW}Inget commit-meddelande angivet. Använder:${NC} $MSG"
fi

# Bekräftelse
echo ""
read -p "$(echo -e ${GREEN}Committa och pusha med meddelande \"$MSG\"? [y/N]: ${NC})" CONFIRM
if [[ ! "$CONFIRM" =~ ^[yY]$ ]]; then
    echo -e "${RED}Avbruten.${NC}"
    exit 1
fi

# Add, commit, push
echo ""
echo -e "${BLUE}● git add -A${NC}"
git add -A

echo -e "${BLUE}● git commit${NC}"
git commit -m "$MSG"

echo -e "${BLUE}● git push origin $BRANCH${NC}"
git push origin "$BRANCH"

echo ""
echo -e "${GREEN}✓ Klar!${NC} Railway plockar upp pushen och deployar automatiskt."
echo -e "  Följ bygget på: https://railway.app/dashboard"
