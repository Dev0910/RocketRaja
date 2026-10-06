import { PremiumError } from './authority.mjs';

// getAccessToken is server credential plumbing, never an APK key or a client-supplied token.
export class GooglePlayVerifier {
  constructor(getAccessToken, request = fetch) { this.getAccessToken = getAccessToken; this.request = request; }
  async verify(packageName, productId, purchaseToken) {
    const parts = [packageName, productId, purchaseToken].map(encodeURIComponent);
    const url = `https://androidpublisher.googleapis.com/androidpublisher/v3/applications/${parts[0]}/purchases/products/${parts[1]}/tokens/${parts[2]}`;
    const token = await this.getAccessToken();
    if (typeof token !== 'string' || !token) throw new PremiumError('verification-unavailable');
    const response = await this.request(url, { headers: { Authorization: 'Bearer ' + token, Accept: 'application/json' }, signal: AbortSignal.timeout(15000) });
    if (!response.ok) throw new PremiumError(response.status === 404 || response.status === 400 ? 'purchase-invalid' : 'verification-unavailable');
    return response.json();
  }
}
