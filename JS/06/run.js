var fs = require('fs');

function parseInput() {
  const content = fs.readFileSync('./input', 'utf8');
  return content.split('\n').map(line => {
    const [orbitant, orbiter] = line.split(')');
    return { orbitant, orbiter };
  });
}

function getGraph(input) {
  const nodes = [];
  const edges = [];

  input.forEach(i => {
    if (!nodes.some(n => n === i.orbitant)) {
      nodes.push(i.orbitant);
    }
    if (!nodes.some(n => n === i.orbiter)) {
      nodes.push(i.orbiter);
    }
    edges.push({ from: i.orbitant, to: i.orbiter });
  });

  return { nodes, edges };
}

function countDirectOrbits(graph) {
  return graph.edges.length;
}

function getOrbitalPath(graph, node) {
  const results = [node];
  let edge = graph.edges.find(e => e.to === node);
  if (edge) {
    let current = edge.from;
    while (current) {
      results.push(current);
      edge = graph.edges.find(e => e.to === current);
      if (edge) {
        current = edge.from;
      } else {
        current = undefined;
      }
    }
  }
  return results;
}

function countIndirectOrbits(graph) {
  let count = 0;
  graph.nodes.forEach(node => {
    const path = getOrbitalPath(graph, node);
    count += Math.max(path.length - 2, 0);
  });
  return count;
}


function part1() {
  const input = parseInput();
  const graph = getGraph(input);
  const directOrbits = countDirectOrbits(graph);
  const indirectOrbits = countIndirectOrbits(graph);
  return directOrbits + indirectOrbits;
}

function part2() {
  const input = parseInput();
  const graph = getGraph(input);
  const you = 'YOU';
  const santa = 'SAN';
  const youPath = getOrbitalPath(graph, you);
  const santaPath = getOrbitalPath(graph, santa);

  for (let youPathIndex = 0; youPathIndex < youPath.length; youPathIndex++) {
    const node = youPath[youPathIndex];
    const santaPathIndex = santaPath.findIndex(n => n === node);
    if (santaPathIndex !== -1) {
      return youPathIndex + santaPathIndex - 2;
    }
  }
}

const p1 = part1();
console.log(`Part 1: ${p1}`);
const p2 = part2();
console.log(`Part 2: ${p2}`);

