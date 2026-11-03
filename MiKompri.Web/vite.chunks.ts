// Keep the initial dependency closure independent of deferred UI modules.
// Production and the performance laboratory must use the same boundaries.
export const presentationChunks = {
  groups: [{
    name: 'initial',
    tags: ['$initial' as const],
  }],
}
