import React from 'react';
import clsx from 'clsx';
import styles from './HomepageFeatures.module.css';

const FeatureList = [
  {
    title: 'Modularity',
    description: (
      <>
        Mirage is built on top of modular components and is easily extensible. Use only the features that you need.
      </>
    ),
  },
  {
    title: 'Shared codebase',
    description: (
      <>
        Both server and client can share the same code in a single project which improves productivity and simplifies development.
      </>
    ),
  },
  {
    title: 'No static state',
    description: (
      <>
        Run multiple servers/clients from a single Unity instance.
      </>
    ),
  },
  {
    title: 'Low bandwidth usage',
    description: (
      <>
        Use our <a href="docs/guides/bit-packing/">Bit Packing</a> features to greatly reduce bandwidth.
      </>
    ),
  },
  {
    title: 'State synchronization',
    description: (
      <>
        Use <a href="docs/guides/sync/">SyncVars</a> to easily synchronize your game state over the network.
      </>
    ),
  },
  {
    title: 'Server authoritative',
    description: (
      <>
        Utilize the <a href="docs/guides/remote-actions/server-rpc">Server RPCs</a> to do server-authoritative tasks.
      </>
    ),
  }
];

function Feature({Svg, title, description}) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures() {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
