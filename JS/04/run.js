var fs = require('fs');

function parseInput() {
  const content = fs.readFileSync('./input', 'utf8');
  return content.split('-').map(code => parseInt(code));
}

function hasLengthSix(password) {
  return password.length === 6;
}

function hasNonDecreasingCharacters(password) {
  return /^0{0,6}1{0,6}2{0,6}3{0,6}4{0,6}5{0,6}6{0,6}7{0,6}8{0,6}9{0,6}$/.test(password);
}

function hasADoubleCharacter(password) {
  return /(00|11|22|33|44|55|66|77|88|99)/.test(password);
}

function hasADoubleButNotTripleCharacter(password) {
  const m = password.match(/(1{2,}|2{2,}|3{2,}|4{2,}|5{2,}|6{2,}|7{2,}|8{2,}|9{2,})/g);
  return m && m.some(m => m.length === 2);
}

function validate(password) {
  return hasLengthSix(password)
    && hasNonDecreasingCharacters(password)
    && hasADoubleCharacter(password);
}

function validate2(password) {
  return hasLengthSix(password)
    && hasNonDecreasingCharacters(password)
    && hasADoubleButNotTripleCharacter(password);
}

function part1() {
  const range = parseInput();
  let count = 0;
  for (let i = range[0]; i <= range[1]; i++) {
    let password = i.toString();
    if (validate(password)) {
      count++;
    }
  }
  return count;
}

function part2() {
  const range = parseInput();
  let count = 0;
  for (let i = range[0]; i <= range[1]; i++) {
    let password = i.toString();
    if (validate2(password)) {
      count++;
    }
  }
  return count;
}

const p1 = part1();
console.log(`Part 1: ${p1}`);

const p2 = part2();
console.log(`Part 2: ${p2}`);