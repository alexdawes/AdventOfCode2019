var fs = require('fs');

function parseInput() {
  const content = fs.readFileSync('./input', 'utf8');
  return content.split(',').map(code => parseInt(code));
}

function runProgram(buffer, pointer) {
  while (true) {
    const opCode = buffer[pointer];
    switch (opCode) {
      case 1: {
        const operand1Pointer = buffer[pointer+1];
        const operand2Pointer = buffer[pointer+2];
        const resultPointer = buffer[pointer+3];
        buffer[resultPointer] = buffer[operand1Pointer] + buffer[operand2Pointer];
        pointer += 4;
        break;
      }
      case 2: {
        const operand1Pointer = buffer[pointer+1];
        const operand2Pointer = buffer[pointer+2];
        const resultPointer = buffer[pointer+3];
        buffer[resultPointer] = buffer[operand1Pointer] * buffer[operand2Pointer];
        pointer += 4;
        break;
      }
      case 99: {
        return;
      }
      default: {
        throw `Unrecognised opcode: ${opCode}`;
      }
    }
  }
}

function run(noun, verb) {
  let pointer = 0;
  let buffer = parseInput();
  buffer[1] = noun;
  buffer[2] = verb;
  runProgram(buffer, pointer);
  return buffer[0];
}

function part1() {
  return run(12, 2);
}

function part2() {
  for (let i = 0; i < 100; i++) {
    for (let j = 0; j < 100; j++) {
      let result = run(i,j);
      if (result === 19690720) {
        return (100 * i) + j;
      }
    }
  }
}

const p1 = part1();
console.log(`Part 1: ${p1}`);
const p2 = part2();
console.log(`Part 2: ${p2}`);