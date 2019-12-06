var fs = require('fs');

class PathSegment {
  constructor(start, end) {
    this.start = start;
    this.end = end;
  }

  minX() { return Math.min(this.start.x, this.end.x); }
  maxX() { return Math.max(this.start.x, this.end.x); }
  minY() { return Math.min(this.start.y, this.end.y); }
  maxY() { return Math.max(this.start.y, this.end.y); }

  intersects(other) {
    return this.minX() <= other.maxX()
        && this.maxX() >= other.minX()
        && this.minY() <= other.maxY()
        && this.maxY() >= other.minY();
  }

  contains(point) {
    return this.getPoints().some(p => p.x === point.x && p.y === point.y);
  }

  getPoints() {
    const vertical = this.start.x === this.end.x;
    if (vertical) {
      const x = this.start.x;
      const path = [];
      const step = this.start.y < this.end.y ? 1 : -1;
      let y = this.start.y;
      while(true) {
        path.push({ x, y });
        if (y === this.end.y) {
          break;
        }
        y += step;
      }
      return path;
    }
    else {
      const y = this.start.y;
      const path = [];
      const step = this.start.x < this.end.x ? 1 : -1;
      let x = this.start.x;
      while(true) {
        path.push({ x, y });
        if (x === this.end.x) {
          break;
        }
        x += step;
      }
      return path;
    }
  }

  getIntersection(other) {
    if (!this.intersects(other)) {
      return [];
    }

    const path = this.getPoints();
    const otherPath = other.getPoints();

    return path.filter(p1 => otherPath.some(p2 => p1.x === p2.x && p1.y === p2.y));
  }
}

function parseInstruction(inst) {
  const direction = inst[0];
  const distance = parseInt(inst.substring(1));
  return { direction, distance };
}

function parseInput() {
  const content = fs.readFileSync('./input', 'utf8');
  return content.split('\n').map(line => line.split(',').map(parseInstruction));
}

function step(position, instruction) {
  switch (instruction.direction) {
    case 'L': {
      return { x: position.x - instruction.distance, y: position.y };
    }
    case 'R': {
      return { x: position.x + instruction.distance, y: position.y };
    }
    case 'U': {
      return { x: position.x, y: position.y - instruction.distance };
    }
    case 'D': {
      return { x: position.x, y: position.y + instruction.distance };
    }
    default: {
      throw `Unrecognised direction: ${direction}`;
    }
  }
}

function l1(coord1, coord2) {
  return Math.abs(coord1.x - coord2.x) + Math.abs(coord1.y - coord2.y);
}

function newNorm (coord, path) {
  let result = 0;
  for (let i = 0; i < path.length; i++) {
    const segment = path[i];
    if (segment.contains(coord)) {
      const points = segment.getPoints();
      for (var j = 1; j < points.length; j++) {
        result++;
        if (points[j].x === coord.x && points[j].y === coord.y) {
          return result;
        }
      }
    }
    else {
      result += segment.getPoints().length - 1;
    }
  }
  throw 'here';
}

function getSegments(instructions) {
  const segments = [];
  let start = { x: 0, y: 0 };
  instructions.forEach((instruction) => {
    const end = step(start, instruction);
    segments.push(new PathSegment(start, end));
    start = end;
  });
  return segments;
}

function getIntersection(segments1, segments2) {
  return segments1.reduce((r1, s1) => {
    return r1.concat(segments2.reduce((r2, s2) => {
      return r2.concat(s1.getIntersection(s2))
    }, []));
  }, []);
}

function part1() {
  const input = parseInput();
  const path1 = getSegments(input[0]);
  const path2 = getSegments(input[1]);
  const intersections = getIntersection(path1, path2);
  const withoutStart = intersections.filter(i => i.x || i.y);
  const minDistance = Math.min(...withoutStart.map(pos => l1(pos, {x:0,y:0})));
  return minDistance;
}

function part2() {
  const input = parseInput();
  const path1 = getSegments(input[0]);
  const path2 = getSegments(input[1]);
  const intersections = getIntersection(path1, path2);
  const withoutStart = intersections.filter(i => i.x || i.y);
  const minDistance = Math.min(...withoutStart.map(pos => newNorm(pos, path1) + newNorm(pos, path2)));
  return minDistance;
}

const p1 = part1();
console.log(`Part 1: ${p1}`);

const p2 = part2();
console.log(`Part 2: ${p2}`);