export type ExternalAccountFlow = 'account-link' | 'account-step-up';

const FLOW_KEY = 'changeMe.externalAccountFlow';

export function storeExternalAccountFlow(flow: ExternalAccountFlow): void {
  sessionStorage.setItem(FLOW_KEY, flow);
}

export function readExternalAccountFlow(): ExternalAccountFlow | null {
  const value = sessionStorage.getItem(FLOW_KEY);
  if (value === 'account-link' || value === 'account-step-up') {
    return value;
  }
  return null;
}

export function clearExternalAccountFlow(): void {
  sessionStorage.removeItem(FLOW_KEY);
}
