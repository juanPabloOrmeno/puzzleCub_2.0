import { readFileSync, writeFileSync } from "node:fs";

const path = "Assets/Resources/Levels/levels.json";
const originalLevels = JSON.parse(readFileSync(path, "utf8")).levels;
const colors = ["Red", "Yellow", "Green", "Cyan", "Orange", "Magenta", "Teal", "Indigo", "Blanc"];

function difficulty(level) {
    const pieces = level.pieces.length;
    const obstacles = level.tiles.filter((tile) => tile.type === "Obstacle").length;
    const colorCount = new Set(level.pieces.map((piece) => piece.type)).size;
    return pieces * 2 + obstacles + colorCount * 2;
}

function makeGeneratedLevel(level) {
    const pairCount = 1 + Math.floor((level - 1) / 28);
    const width = Math.min(9, 3 * Math.min(3, 1 + Math.floor((level - 1) / 145)));
    const pairsPerShelf = width / 3;
    const shelfCount = Math.ceil(pairCount / pairsPerShelf);
    const height = shelfCount * 2 - 1;
    const palette = colors.slice(0, Math.min(colors.length, 2 + Math.floor((level - 1) / 65)));
    const pieces = [];
    const tiles = [];

    // Obstacles separate the shelves, so every visible pair remains a valid horizontal puzzle.
    for (let y = 1; y < height; y += 2) {
        for (let x = 0; x < width; x++) {
            tiles.push({ type: "Obstacle", x, y });
        }
    }

    for (let pairIndex = 0; pairIndex < pairCount; pairIndex++) {
        const shelf = Math.floor(pairIndex / pairsPerShelf);
        const column = pairIndex % pairsPerShelf;
        const type = palette[(pairIndex + level) % palette.length];
        const x = column * 3;
        const y = shelf * 2;

        pieces.push({ type, x, y });
        pieces.push({ type, x: x + 2, y });
    }

    return {
        level,
        width,
        height,
        borderSize: width >= 9 ? 0.75 : 1.0,
        time: Math.max(80, 180 - Math.floor((level - 1) / 50) * 10),
        pieces,
        tiles
    };
}

const rankedOriginals = [...originalLevels].sort((a, b) => {
    const difference = difficulty(a) - difficulty(b);
    return difference !== 0 ? difference : a.level - b.level;
});
const minimumDifficulty = difficulty(rankedOriginals[0]);
const maximumDifficulty = difficulty(rankedOriginals.at(-1));
const originalSlots = new Map();

for (const original of rankedOriginals) {
    const normalized = (difficulty(original) - minimumDifficulty) / (maximumDifficulty - minimumDifficulty || 1);
    let slot = 15 + Math.round(normalized * 180);
    while (originalSlots.has(slot)) {
        slot++;
    }

    originalSlots.set(slot, {
        ...original,
        tiles: original.tiles.filter((tile, index, tiles) => tiles.findIndex((candidate) => candidate.x === tile.x && candidate.y === tile.y) === index)
    });
}

const levels = [];
for (let level = 1; level <= 500; level++) {
    const original = originalSlots.get(level);
    levels.push(original ? { ...original, level } : makeGeneratedLevel(level));
}

writeFileSync(path, `${JSON.stringify({ levels }, null, 2)}\n`);
