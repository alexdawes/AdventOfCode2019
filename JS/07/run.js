var fs = require('fs');

function parseInput() {
  const content = fs.readFileSync('./input', 'utf8');
  return content.split(',').map(code => parseInt(code));
}

function parseInstruction(instruction) {
  const opCode = instruction % 100;
  const modes = [];
  let p = Math.floor(instruction / 100);
  while (p) {
    modes.push(p % 10);
    p = Math.floor(p / 10);
  }
  return { opCode, modes };
}

function getParameterValue(buffer, pointer, mode) {
  mode = mode || 0;
  switch (mode) {
    case 0: {
      return buffer[buffer[pointer]];
    }
    case 1: {
      return buffer[pointer];
    }
    default: {
      throw `Unrecognised parameter mode: ${mode}`;
    }
  }
}

function getReadParameters(buffer, pointer, count, modes) {
  return new Array(count).fill(undefined).map((_, idx) => getParameterValue(buffer, pointer+idx+1, modes[idx]));
}

function getWriteParameter(buffer, pointer) {
  return buffer[pointer];
}

function log(buffer, pointer, count) {
  console.log(buffer.slice(pointer, pointer + count));
}

const returnCodes = {
  COMPLETE: 0,
  AWAITING_INPUT: 1
};

function runProgram(buffer, pointer, input, output) {
  while(true) {
    const instruction = parseInstruction(buffer[pointer]);
    switch (instruction.opCode) {
      case 1: {
        log(buffer, pointer, 4);
        const [o1, o2] = getReadParameters(buffer, pointer, 2, instruction.modes);
        const r = getWriteParameter(buffer, pointer + 3);
        buffer[r] = o1 + o2;
        pointer += 4;
        break;
      }
      case 2: {
        log(buffer, pointer, 4);
        const [o1, o2] = getReadParameters(buffer, pointer, 2, instruction.modes);
        const r = getWriteParameter(buffer, pointer + 3);
        buffer[r] = o1 * o2;
        pointer += 4;
        break;
      }
      case 3: {
        log(buffer, pointer, 2);
        if (input.length === 0) {
          return returnCodes.AWAITING_INPUT;
        }
        const i = input.shift();
        const r = getWriteParameter(buffer, pointer + 1);
        buffer[r] = i;
        pointer += 2;
        break;
      }
      case 4: {
        log(buffer, pointer, 2);
        const [r] = getReadParameters(buffer, pointer, 1, instruction.modes);
        output.push(r);
        pointer += 2;
        break;
      }
      case 5: {
        log(buffer, pointer, 3);
        const [p1, p2] = getReadParameters(buffer, pointer, 2, instruction.modes);
        if (!!p1) {
          pointer = p2;
        } else {
          pointer += 3;
        }
        break;
      }
      case 6: {
        log(buffer, pointer, 3);
        const [p1, p2] = getReadParameters(buffer, pointer, 2, instruction.modes);
        if (!p1) {
          pointer = p2;
        } else {
          pointer += 3;
        }
        break;
      }
      case 7: {
        log(buffer, pointer, 4);
        const [c1, c2] = getReadParameters(buffer, pointer, 2, instruction.modes);
        const r = getWriteParameter(buffer, pointer + 3);
        buffer[r] = (c1 < c2 ? 1 : 0);
        pointer += 4;
        break;
      }
      case 8: {
        log(buffer, pointer, 4);
        const [c1, c2] = getReadParameters(buffer, pointer, 2, instruction.modes);
        const r = getWriteParameter(buffer, pointer + 3);
        buffer[r] = (c1 === c2 ? 1 : 0);
        pointer += 4;
        break;
      }
      case 99: {
        // console.log(buffer.slice(pointer, pointer + 1), buffer);
        return returnCodes.COMPLETE;
      }
      default: {
        throw `Unrecognised opcode: ${instruction.opCode}`;
      }
    }
  }
}

function run(input, output) {
  let pointer = 0;
  const buffer = parseInput();
  return runProgram(buffer, pointer, input, output);
}

function getPermutations(options) {
  if (options.length === 0) { 
    return [[]];
  }
  return options.reduce((lst, o, oIdx) => {
    return lst.concat(getPermutations(options.filter((_, idx) => idx !== oIdx)).map(l => [o].concat(l)));
  }, []);
}

function runForPermutation(permutation) {
  const streamA = [permutation[0], 0];
  const streamB = [permutation[1]];
  const streamC = [permutation[2]];
  const streamD = [permutation[3]];
  const streamE = [permutation[4]];

}

function part1() {
  const output = run([1]);
  return output[output.length - 1];
}

function part2() {
  const output = run([5]);
  return output[output.length - 1];
}

const p1 = part1();
console.log(`Part 1: ${p1}`);
const p2 = part2();
console.log(`Part 2: ${p2}`);