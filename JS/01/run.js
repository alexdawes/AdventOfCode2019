var fs = require('fs');

function parseInput() {
  const content = fs.readFileSync('./input', 'utf8');
  return content.split('\n').map(line => parseInt(line));
}

function getFuel(mass) {
  return Math.max(Math.floor(mass / 3) - 2, 0);
}

function getFuelMass(mass) {
  let total = 0;
  let fuel = getFuel(mass);
  while (fuel != 0) {
    total += fuel;
    fuel = getFuel(fuel);
  }
  return total;
}

function part1() {
  var input = parseInput();
  var fuels = input.map(i => getFuel(i));
  var total = fuels.reduce((i, j) => i+j, 0);
  return total;
}

function part2() {
  var input = parseInput();
  var fuels = input.map(i => getFuelMass(i));
  var total = fuels.reduce((i, j) => i+j, 0);
  return total;
}

const p1 = part1();
console.log(`Part 1: ${p1}`);
const p2 = part2(p1);
console.log(`Part 2: ${p2}`);